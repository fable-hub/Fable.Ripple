import { defineConfig } from 'vite'

// https://vitejs.dev/config/
export default defineConfig((env) => {
    const isDevelpoment = env.mode === 'development';

    return {
        css: {
            devSourcemap: isDevelpoment
        },
        plugins: [],
        build: {
            rollupOptions: {
                output: {
                    // Fixed, not hashed: the docs site links this path directly from
                    // demo.md, which cannot know a hash chosen at build time.
                    entryFileNames: 'assets/main.js',
                    assetFileNames: (chunk) =>
                        chunk.names?.some((name) => name.endsWith('.css'))
                            ? 'assets/main.css'
                            : 'assets/[name]-[hash][extname]'
                }
            }
        },
        server: {
            host: true
            // `.fs` files are deliberately NOT ignored here. They used to be -
            // presumably so that an edit reloaded once, after Fable had written
            // the matching `.fs.js`, rather than twice. But the code panel loads
            // those same `.fs` files as content (`?raw`, see demo/Sources.fs),
            // so ignoring them meant editing an example never refreshed its
            // listing and you had to restart the server to see a change.
            // An extra reload is a much smaller price than that.
        },
        clearScreen: false
    }
})
