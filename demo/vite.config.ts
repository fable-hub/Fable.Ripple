import { defineConfig } from "vite";

// Kept in sync with the import line in styles/app.css.
const STANDALONE_IMPORT = '@import "./standalone.css";';

// https://vitejs.dev/config/
export default defineConfig((env) => {
    const isDevelpoment = env.mode === "development";

    return {
        css: {
            devSourcemap: isDevelpoment,
        },
        plugins: [
            {
                name: "drop-standalone-css",
                apply: "build",
                enforce: "pre",
                transform(code, id) {
                    if (id.includes("styles/app.css")) {
                        if (!code.includes(STANDALONE_IMPORT)) {
                            this.error(
                                `expected ${STANDALONE_IMPORT} in app.css`,
                            );
                        }
                        return code.replace(STANDALONE_IMPORT, "");
                    }
                },
            },
        ],
        build: {
            rollupOptions: {
                output: {
                    // Fixed, not hashed: the docs site links this path directly from
                    // demo.md, which cannot know a hash chosen at build time.
                    entryFileNames: "assets/main.js",
                    assetFileNames: (chunk) =>
                        chunk.names?.some((name) => name.endsWith(".css"))
                            ? "assets/main.css"
                            : "assets/[name]-[hash][extname]",
                },
            },
        },
        server: {
            host: true,
            watch: {
                // A .fs change makes Vite full-reload, which discards the hot update.
                ignored: ["**/*.fs"]
            }
        },
        clearScreen: false,
    };
});
