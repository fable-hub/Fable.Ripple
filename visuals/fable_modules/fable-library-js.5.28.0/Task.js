import { OperationCanceledException } from "./AsyncBuilder.js";
export class TaskCompletionSource {
    constructor() {
        this.task = new Promise((resolve, reject) => {
            this._resolve = resolve;
            this._reject = reject;
        });
    }
    SetResult(value) { this._resolve(value); }
    SetException(error) { this._reject(error); }
    SetCancelled() { this._reject(new OperationCanceledException()); }
    get_Task() { return this.task; }
}
export function fromResult(value) {
    return Promise.resolve(value);
}
export function zero() {
    return Promise.resolve();
}
// Like .NET, -1 waits until the token is cancelled
export function delay(millisecondsDelay, cancellationToken) {
    return new Promise((resolve, reject) => {
        if (cancellationToken?.isCancelled) {
            reject(new OperationCanceledException());
            return;
        }
        let tokenId;
        const timeoutId = millisecondsDelay === -1 ? undefined : setTimeout(() => {
            if (tokenId !== undefined) {
                cancellationToken.removeListener(tokenId);
            }
            resolve();
        }, millisecondsDelay);
        if (cancellationToken != null) {
            tokenId = cancellationToken.addListener(() => {
                clearTimeout(timeoutId);
                reject(new OperationCanceledException());
            });
        }
    });
}
// Task<T> = Promise<T> in JS/TS. GetAwaiter/GetResult return the Promise itself;
// callers should use Async.AwaitTask to extract the value in an async context.
export function getAwaiter(t) {
    return t;
}
export function getResult(t) {
    return t;
}
