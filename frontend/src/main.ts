import { createApp } from "vue";
import "bootstrap-icons/font/bootstrap-icons.css";
import "video.js";
import "video.js/dist/video-js.css";
import app from "@/app.vue";
import { router } from "@/router";
import { pinia } from "@/store";
import "@/style.css";
import { refreshLoginStatus } from "@/utils/auth";

createApp(app).use(router).use(pinia).mount("#app");

refreshLoginStatus();
