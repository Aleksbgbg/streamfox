import { createApp } from "vue";
import "video.js";
import "video.js/dist/video-js.css";
import App from "@/App.vue";
import "@/assets/main.css";
import { router } from "@/router";

createApp(App).use(router).mount("#app");
