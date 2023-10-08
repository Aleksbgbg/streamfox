import { createRouter, createWebHistory } from "vue-router";

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: "/",
      name: "home",
      component: () => import("@/views/home.vue"),
    },
    {
      path: "/register",
      name: "register",
      component: () => import("@/views/accounts/register.vue"),
    },
    {
      path: "/login",
      name: "login",
      component: () => import("@/views/accounts/login.vue"),
    },
    {
      path: "/watch/:id",
      name: "watch",
      component: () => import("@/views/video/watch.vue"),
    },
    {
      path: "/upload",
      name: "upload",
      component: () => import("@/views/video/manage/upload.vue"),
    },
    {
      path: "/edit/:videoId",
      name: "edit",
      component: () => import("@/views/video/manage/edit.vue"),
      props: true,
    },
  ],
});
