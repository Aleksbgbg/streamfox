<script>
export default {
  props: {
    videoId: String,
  },
};
</script>

<template lang="pug">
router-link(:to="{ name: 'watch', params: { id: videoId } }")
  slot
</template>
