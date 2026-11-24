import { OperationCanceledException, Trampoline } from "./AsyncBuilder.js";
import { CancellationToken } from "./AsyncBuilder.js";
import { protectedCont, protectedBind, protectedReturn } from "./AsyncBuilder.js";
import { Choice_makeChoice1Of2, Choice_makeChoice2Of2 } from "./Choice.js";
import { ArgumentException_$ctor_Z384F8060, InvalidOperationException_$ctor_Z721C83C5, TimeoutException_$ctor } from "./System.js";
import { Exception, ensureErrorOrException } from "./Util.js";
function emptyContinuation(_x) {
    // NOP
}
// MakeAsync: body:(AsyncActivation<'T> -> AsyncReturn) -> Async<'T>
export function makeAsync(body) {
    return body;
}
// Invoke: computation: Async<'T> -> ctxt:AsyncActivation<'T> -> AsyncReturn
export function invoke(computation, ctx) {
    return computation(ctx);
}
// CallThenInvoke: ctxt:AsyncActivation<'T> -> result1:'U -> part2:('U -> Async<'T>) -> AsyncReturn
export function callThenInvoke(ctx, result1, part2) {
    return part2(result1)(ctx);
}
// Bind: ctxt:AsyncActivation<'T> -> part1:Async<'U> -> part2:('U -> Async<'T>) -> AsyncReturn
export function bind(ctx, part1, part2) {
    return protectedBind(part1, part2)(ctx);
}
export function createCancellationToken(arg) {
    const token = new CancellationToken(typeof arg === "boolean" ? arg : false);
    if (typeof arg === "number") {
        setTimeout(() => { token.cancel(); }, arg);
    }
    return token;
}
export function cancel(token) {
    token.cancel();
}
export function cancelAfter(token, ms) {
    setTimeout(() => { token.cancel(); }, ms);
}
export function isCancellationRequested(token) {
    return token != null && token.isCancelled;
}
export function throwIfCancellationRequested(token) {
    if (token != null && token.isCancelled) {
        throw new Exception("Operation is cancelled");
    }
}
// The parent keeps the listener until unlink is called
function createLinkedToken(parent) {
    const token = new CancellationToken();
    const id = parent.addListener(() => token.cancel());
    return [token, () => { parent.removeListener(id); }];
}
export function startChild(computation, ms) {
    return protectedCont((ctx) => {
        const timeout = ms == null || ms === -1 ? undefined : ms;
        const [childToken, unlink] = timeout === undefined
            ? [ctx.cancelToken, () => { }]
            : createLinkedToken(ctx.cancelToken);
        const promise = startAsPromise(computation, childToken);
        let settled = false;
        const onSettled = () => { settled = true; unlink(); };
        // Without a rejection handler an unawaited child's failure is an unhandled rejection
        promise.then(onSettled, onSettled);
        // Like .NET, the timeout only runs while the result is awaited, not from the start of the child
        const awaitResult = () => timeout === undefined || settled ? promise : new Promise((resolve, reject) => {
            const timeoutId = setTimeout(() => {
                // .NET doesn't cancel the child when the timeout is 0
                if (timeout !== 0) {
                    try {
                        childToken.cancel();
                    }
                    catch (error) {
                        reject(error);
                        return;
                    }
                }
                reject(TimeoutException_$ctor());
            }, timeout);
            promise.then(value => { clearTimeout(timeoutId); resolve(value); }, error => { clearTimeout(timeoutId); reject(error); });
        });
        // JS Promises are hot, computation has already started
        // but we delay returning the result
        protectedReturn(protectedCont((awaitCtx) => awaitPromise(awaitResult())(awaitCtx)))(ctx);
    });
}
export function awaitPromise(p) {
    return fromContinuations((conts) => p.then(conts[0]).catch((err) => (err instanceof OperationCanceledException
        ? conts[2] : conts[1])(err)));
}
export function awaitEvent(event, cancelAction) {
    return protectedCont((ctx) => {
        let tokenId;
        const handler = ((_sender, arg) => {
            ctx.cancelToken.removeListener(tokenId);
            event.RemoveHandler(handler);
            ctx.onSuccess(arg);
        });
        tokenId = ctx.cancelToken.addListener(() => {
            event.RemoveHandler(handler);
            if (cancelAction != null) {
                cancelAction();
            }
            ctx.onCancel(new OperationCanceledException());
        });
        if (ctx.cancelToken.isCancelled) {
            return;
        }
        event.AddHandler(handler);
    });
}
export function cancellationToken() {
    return protectedCont((ctx) => ctx.onSuccess(ctx.cancelToken));
}
export const defaultCancellationToken = new CancellationToken();
export function catchAsync(work) {
    return protectedCont((ctx) => {
        work({
            onSuccess: (x) => ctx.onSuccess(Choice_makeChoice1Of2(x)),
            onError: (ex) => ctx.onSuccess(Choice_makeChoice2Of2(ex)),
            onCancel: ctx.onCancel,
            cancelToken: ctx.cancelToken,
            trampoline: ctx.trampoline,
        });
    });
}
export function tryCancelled(computation, compensation) {
    return (ctx) => {
        // invariant: install the handler without adding a cancellation checkpoint, including for nested handlers
        computation({
            ...ctx,
            onCancel: (error) => {
                // decision: preserve the original cancellation even when compensation throws, matching .NET
                try {
                    compensation(error);
                }
                catch { /* Cancellation takes precedence. */ }
                if (ctx.trampoline.incrementAndCheck()) {
                    ctx.trampoline.hijack(() => ctx.onCancel(error));
                }
                else {
                    ctx.onCancel(error);
                }
            },
        });
    };
}
export function fromContinuations(f) {
    return (ctx) => {
        let settled = false;
        const claim = () => {
            if (settled) {
                throw InvalidOperationException_$ctor_Z721C83C5("A continuation was invoked more than once");
            }
            settled = true;
        };
        const success = (x) => { claim(); ctx.onSuccess(x); };
        const error = (x) => { claim(); ctx.onError(x); };
        const cancelled = (x) => { claim(); ctx.onCancel(x); };
        protectedCont(() => f([success, error, cancelled]))({
            onSuccess: success,
            onError: (x) => { if (settled) {
                throw x;
            } error(x); },
            onCancel: cancelled,
            cancelToken: ctx.cancelToken,
            trampoline: ctx.trampoline,
        });
    };
}
export function ignore(computation) {
    return protectedBind(computation, (_x) => protectedReturn(void 0));
}
export function parallel(computations, maxDegreeOfParallelism) {
    if (maxDegreeOfParallelism != null && maxDegreeOfParallelism < 1) {
        throw ArgumentException_$ctor_Z384F8060(`maxDegreeOfParallelism must be positive, was ${maxDegreeOfParallelism}`, "maxDegreeOfParallelism");
    }
    return protectedCont((ctx) => awaitPromise(runParallel(Array.from(computations), maxDegreeOfParallelism, ctx.cancelToken))(ctx));
}
function runParallel(computations, limit, parentToken) {
    return new Promise((resolve, reject) => {
        const count = computations.length;
        const results = new Array(count);
        const [token, unlink] = createLinkedToken(parentToken);
        let next = 0;
        let running = 0;
        let failed = false;
        let failure;
        const startNext = () => {
            const i = next++;
            running++;
            startAsPromise(computations[i], token)
                .then((x) => { results[i] = x; }, (err) => {
                if (!failed) {
                    failed = true;
                    failure = err;
                    try {
                        token.cancel();
                    }
                    catch (error) {
                        failure = error;
                    }
                }
            })
                .then(() => {
                running--;
                if (!failed && next < count) {
                    startNext();
                }
                else if (running === 0) {
                    unlink();
                    if (failed) {
                        reject(failure);
                    }
                    else {
                        resolve(results);
                    }
                }
            });
        };
        if (count === 0) {
            unlink();
            resolve(results);
        }
        const initial = Math.min(limit ?? count, count);
        while (next < initial) {
            startNext();
        }
    });
}
export function choice(computations) {
    return protectedCont((ctx) => {
        const works = Array.from(computations);
        let remaining = works.length;
        if (remaining === 0) {
            ctx.onSuccess(undefined);
            return;
        }
        const [token, unlink] = createLinkedToken(ctx.cancelToken);
        let done = false;
        const finish = (report) => {
            if (done) {
                return;
            }
            done = true;
            unlink();
            try {
                token.cancel();
            }
            catch (error) {
                ctx.onError(ensureErrorOrException(error));
                return;
            }
            report();
        };
        for (const work of works) {
            startWithContinuations(work, (x) => {
                if (x != null) {
                    finish(() => ctx.onSuccess(x));
                }
                else if (--remaining === 0) {
                    finish(() => ctx.onSuccess(undefined));
                }
            }, (err) => finish(() => ctx.onError(err)), (err) => finish(() => ctx.onCancel(err)), token);
            if (done) {
                break;
            }
        }
    });
}
export function onCancel(interruption) {
    return protectedCont((ctx) => {
        const token = ctx.cancelToken;
        let interrupted = false;
        const interrupt = () => {
            if (!interrupted) {
                interrupted = true;
                try {
                    interruption();
                }
                catch { /* Ignored, like .NET Async.OnCancel */ }
            }
        };
        const id = token.addListener(interrupt);
        let disposed = false;
        ctx.onSuccess({
            Dispose() {
                if (disposed) {
                    return;
                }
                disposed = true;
                token.removeListener(id);
                // With use! Dispose runs inside cancel() before this listener gets its turn
                if (token.isCancelled) {
                    interrupt();
                }
            },
        });
    });
}
export function sequential(computations) {
    function _sequential(computations, cancelToken) {
        let pr = Promise.resolve([]);
        for (const c of computations) {
            pr = pr.then(results => startAsPromise(c, cancelToken).then(r => results.concat([r])));
        }
        return pr;
    }
    // Children share the parent's cancellation token, like .NET Async.Sequential
    return protectedCont((ctx) => awaitPromise(_sequential(computations, ctx.cancelToken))(ctx));
}
export function sleep(millisecondsDueTime) {
    return protectedCont((ctx) => {
        let completed = false;
        let tokenId;
        const settle = (cancelled) => {
            if (completed) {
                return;
            }
            completed = true;
            clearTimeout(timeoutId);
            if (tokenId !== undefined) {
                ctx.cancelToken.removeListener(tokenId);
            }
            if (cancelled) {
                ctx.onCancel(new OperationCanceledException());
            }
            else {
                ctx.onSuccess(void 0);
            }
        };
        const timeoutId = setTimeout(() => settle(false), millisecondsDueTime);
        tokenId = ctx.cancelToken.addListener(() => settle(true));
    });
}
export function start(computation, cancellationToken) {
    return startWithContinuations(computation, emptyContinuation, function (err) { throw err; }, emptyContinuation, cancellationToken);
}
export function startImmediate(computation, cancellationToken) {
    return start(computation, cancellationToken);
}
export function startWithContinuations(computation, continuation, exceptionContinuation, cancellationContinuation, cancelToken) {
    const trampoline = new Trampoline();
    // Mark the computation completed as soon as a terminal continuation is entered
    // so protectedCont lets exceptions from continuation code propagate instead of
    // routing them to onError (see Trampoline.completed).
    const done = (cont) => (x) => { trampoline.completed = true; return cont(x); };
    computation({
        onSuccess: done(continuation ? continuation : emptyContinuation),
        onError: done(exceptionContinuation),
        onCancel: done(cancellationContinuation),
        cancelToken: cancelToken ? cancelToken : defaultCancellationToken,
        trampoline,
    });
}
export function startAsPromise(computation, cancellationToken) {
    return new Promise((resolve, reject) => startWithContinuations(computation, resolve, reject, reject, cancellationToken ? cancellationToken : defaultCancellationToken));
}
export function startAsTask(computation, _taskCreationOptions, cancellationToken) {
    return startAsPromise(computation, cancellationToken);
}
export function startImmediateAsTask(computation, cancellationToken) {
    return startAsPromise(computation, cancellationToken);
}
