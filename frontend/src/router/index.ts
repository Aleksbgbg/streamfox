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
      path: "/watch/:id",
      name: "watch",
      component: () => import("@/views/video/watch.vue"),
      props: true,
    },
    {
      path: "/user/:userId",
      name: "user",
      component: () => import("@/views/user.vue"),
      props: true,
    },
    {
      path: "/edit/:videoId",
      name: "edit",
      component: () => import("@/views/video/manage/edit.vue"),
      props: true,
    },
    {
      path: "/upload",
      name: "upload",
      component: () => import("@/views/video/manage/upload.vue"),
    },
    {
      path: "/login",
      name: "login",
      component: () => import("@/views/accounts/login.vue"),
    },
    {
      path: "/register",
      name: "register",
      component: () => import("@/views/accounts/register.vue"),
    },
    {
      path: "/live",
      name: "live",
      component: () => import("@/views/live/live.vue"),
    },
  ],
});
