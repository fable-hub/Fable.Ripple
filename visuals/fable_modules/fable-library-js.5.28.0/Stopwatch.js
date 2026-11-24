// Timestamps are in microseconds, the resolution performance.now() can offer
const ticksPerMillisecond = 1000;
export class Stopwatch {
    constructor() {
        // Milliseconds measured before the last Start
        this.elapsed = 0;
        // performance.now() at the last Start, while running
        this.startedAt = undefined;
    }
}
export function create() {
    return new Stopwatch();
}
export function startNew() {
    const sw = new Stopwatch();
    start(sw);
    return sw;
}
export function start(sw) {
    if (sw.startedAt === undefined) {
        sw.startedAt = performance.now();
    }
}
export function stop(sw) {
    if (sw.startedAt !== undefined) {
        sw.elapsed += performance.now() - sw.startedAt;
        sw.startedAt = undefined;
    }
}
export function reset(sw) {
    sw.elapsed = 0;
    sw.startedAt = undefined;
}
export function restart(sw) {
    sw.elapsed = 0;
    sw.startedAt = performance.now();
}
export function isRunning(sw) {
    return sw.startedAt !== undefined;
}
function rawElapsedMilliseconds(sw) {
    return sw.elapsed + (sw.startedAt === undefined ? 0 : performance.now() - sw.startedAt);
}
// In units of Frequency, like .NET (not TimeSpan ticks)
export function elapsedTicks(sw) {
    return BigInt(Math.trunc(rawElapsedMilliseconds(sw) * ticksPerMillisecond));
}
export function elapsedMilliseconds(sw) {
    return elapsedTicks(sw) / BigInt(ticksPerMillisecond);
}
export function elapsedTotalMilliseconds(sw) {
    return Number(elapsedTicks(sw)) / ticksPerMillisecond;
}
export function frequency() {
    return BigInt(ticksPerMillisecond * 1000);
}
export function isHighResolution() {
    return true;
}
export function getTimestamp() {
    return BigInt(Math.trunc(performance.now() * ticksPerMillisecond));
}
export function getElapsedTotalMilliseconds(startingTimestamp, endingTimestamp) {
    return Number((endingTimestamp ?? getTimestamp()) - startingTimestamp) / ticksPerMillisecond;
}
