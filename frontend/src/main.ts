import { createApp } from "vue";
import "bootstrap-icons/font/bootstrap-icons.css";
import "video.js";
import "video.js/dist/video-js.css";
import App from "@/App.vue";
import { router } from "@/router";
import { pinia } from "@/store";
import "@/style.css";
import { refreshLoginStatus } from "@/utils/auth";

refreshLoginStatus();

createApp(App).use(router).use(pinia).mount("#app");
