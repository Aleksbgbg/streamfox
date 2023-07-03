import { createApp } from "vue";
import "bootstrap-icons/font/bootstrap-icons.css";
import "video.js";
import "video.js/dist/video-js.css";
import App from "@/App.vue";
import { router } from "@/router";
import "@/style.css";

createApp(App).use(router).mount("#app");
