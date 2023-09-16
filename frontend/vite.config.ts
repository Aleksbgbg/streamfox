import vue from "@vitejs/plugin-vue";
import dotenv from "dotenv";
import { resolve } from "path";
import { defineConfig } from "vite";
import checker from "vite-plugin-checker";
import eslint from "vite-plugin-eslint";
import svgLoader from "vite-svg-loader";

dotenv.config();

export default defineConfig({
  plugins: [
    vue(),
    svgLoader(),
    eslint(),
    checker({
      vueTsc: true,
    }),
  ],
  resolve: {
    alias: {
      "@": resolve(__dirname, "src"),
    },
  },
  server: {
    host: "0.0.0.0",
    watch: {
      usePolling: true,
    },
    proxy: {
      "/api": `http://${process.env.DEBUG_FORWARD_HOST}:${process.env.DEBUG_FORWARD_PORT}`,
    },
  },
});
