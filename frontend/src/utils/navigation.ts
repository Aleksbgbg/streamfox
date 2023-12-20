import type { LocationQueryValue, RouteLocationNormalizedLoaded, Router } from "vue-router";

function isValidReturnUrl(
  returnUrl: LocationQueryValue | LocationQueryValue[],
): returnUrl is string {
  return typeof returnUrl === "string";
}

export function returnUrl(route: RouteLocationNormalizedLoaded): string {
  return isValidReturnUrl(route.query.returnUrl) ? route.query.returnUrl : route.fullPath;
}

export function navigateBackOrHome(router: Router, route: RouteLocationNormalizedLoaded) {
  if (isValidReturnUrl(route.query.returnUrl)) {
    router.push({ path: route.query.returnUrl });
  } else {
    router.push({ name: "home" });
  }
}
