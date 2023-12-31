<script setup lang="ts">
import { useAttrs } from "vue";

defineOptions({
  inheritAttrs: false,
});

const attrs = useAttrs();

let element: string;
if (attrs.to) {
  element = "router-link";
} else if (attrs.for) {
  element = "label";
} else {
  element = "button";
}

interface Colors {
  default: string;
  hover: string;
}

interface Props {
  theme?: "blue" | "red" | "purple" | "invisible";
  colors?: Partial<Colors>;
  padding?: "normal" | "small" | "slim";
}

const props = withDefaults(defineProps<Props>(), {
  theme: "blue",
  padding: "normal",
});

const themeColors: Colors = {
  ...{
    blue: {
      default: "bg-frost-blue",
      hover: "hover:bg-frost-deep",
    },
    red: {
      default: "bg-aurora-red",
      hover: "hover:bg-aurora-red-700",
    },
    purple: {
      default: "bg-aurora-purple",
      hover: "hover:bg-aurora-purple-700",
    },
    invisible: {
      default: "bg-transparent",
      hover: "hover:bg-polar-light",
    },
  }[props.theme],
  ...props.colors,
};
</script>

<template lang="pug">
component.rounded.transition.duration-150(
  :is="element"
  v-bind="$attrs"
  :class="[themeColors.default, themeColors.hover, { 'px-4 py-2': padding === 'normal', 'px-2 py-1': padding === 'small' }]"
  role="button"
)
  slot
</template>
