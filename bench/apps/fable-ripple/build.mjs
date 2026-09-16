// Production build: Fable (F# -> JS), then esbuild bundle + minify -> dist/app.js.
// Our library is not published, so this compiles against the local
// Fable.Ripple / Fable.Ripple.Dom projects via the App.fsproj ProjectReference.
import { execSync } from 'child_process';
import * as esbuild from 'esbuild';
import { mkdirSync, copyFileSync } from 'fs';
import { fileURLToPath } from 'url';
import { dirname, resolve } from 'path';

const here = dirname(fileURLToPath(import.meta.url));

execSync('dotnet fable . --noCache', { cwd: here, stdio: 'pipe' });

mkdirSync(resolve(here, 'dist'), { recursive: true });
await esbuild.build({
    entryPoints: [resolve(here, 'src/App.fs.js')],
    bundle: true,
    minify: true,
    format: 'iife',
    outfile: resolve(here, 'dist/app.js'),
});

copyFileSync(resolve(here, '../../harness/index.html'), resolve(here, 'dist/index.html'));
