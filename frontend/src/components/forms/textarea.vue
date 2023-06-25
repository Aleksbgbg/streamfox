<template lang="pug">
.flex.flex-col
  label {{ title }}
  textarea.rounded.focus-outline-none.focus-shadow-outline.border.border-gray-300.rounded-lg.py-2.px-4.block.w-full.appearance-none.leading-normal.text-black.placeholder-black(type="text" :placeholder="placeholder")
</template>

<script>
export default {
  props: {
    title: String,
    placeholder: String,
  },
};
</script>
