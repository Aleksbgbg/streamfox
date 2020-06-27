import Vue from "vue";
import VueRouter, { RouteConfig } from "vue-router";

Vue.use(VueRouter);

const routes: Array<RouteConfig> = [
  {
    path: "/",
    name: "home",
    component: () => import("@/components/home/index.vue")
  },
  {
    path: "/watch/:id",
    name: "watch",
    component: () => import("@/components/watch.vue")
  }
];

export default new VueRouter({
  mode: "history",
  base: process.env.BASE_URL,
  routes
});