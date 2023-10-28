<script setup lang="ts">
interface Props {
  center?: boolean;
  errors?: string[];
}
withDefaults(defineProps<Props>(), {
  center: false,
});
</script>

<template lang="pug">
div(:class="{ 'text-center': center }" v-show="errors && errors.length > 0")
  p.text-aurora-red-400(v-for="error of errors") {{ error }}
</template>
