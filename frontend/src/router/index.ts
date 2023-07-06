import * as VueRouter from "vue-router";

export const router = VueRouter.createRouter({
  history: VueRouter.createWebHistory(),
  routes: [
    {
      path: "/",
      name: "home",
      component: () => import("@/components/home/index.vue"),
    },
    {
      path: "/login",
      name: "login",
      component: () => import("@/views/accounts/login.vue"),
    },
    {
      path: "/watch/:id",
      name: "watch",
      component: () => import("@/components/watch.vue"),
    },
    {
      path: "/upload",
      name: "upload",
      component: () => import("@/components/upload/index.vue"),
    },
  ],
});
