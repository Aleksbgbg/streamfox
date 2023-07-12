import * as VueRouter from "vue-router";

export const router = VueRouter.createRouter({
  history: VueRouter.createWebHistory(),
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
      component: () => import("@/views/watch.vue"),
    },
    {
      path: "/upload",
      name: "upload",
      component: () => import("@/views/upload.vue"),
    },
  ],
});
