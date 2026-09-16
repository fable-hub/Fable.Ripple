// Production build: bundle + minify src/app.js -> dist/app.js, copy the shared shell.
import * as esbuild from 'esbuild';
import { mkdirSync, copyFileSync } from 'fs';
import { fileURLToPath } from 'url';
import { dirname, resolve } from 'path';

const here = dirname(fileURLToPath(import.meta.url));
mkdirSync(resolve(here, 'dist'), { recursive: true });

await esbuild.build({
    entryPoints: [resolve(here, 'src/app.js')],
    bundle: true,
    minify: true,
    format: 'esm',
    outfile: resolve(here, 'dist/app.js'),
});

copyFileSync(resolve(here, '../../harness/index.html'), resolve(here, 'dist/index.html'));
