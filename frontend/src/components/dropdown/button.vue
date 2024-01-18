<script setup lang="ts">
import CButton from "@/components/button.vue";
import CDropdownArrow from "@/components/dropdown/arrow.vue";
import CDropdown from "@/components/dropdown/dropdown.vue";

defineOptions({
  inheritAttrs: false,
});
</script>

<template lang="pug">
c-dropdown
  slot(name="dropdown")
  template(#button="{ toggled }")
    c-button.flex.w-full(:class="{ 'rounded-b-none': toggled }" v-bind="$attrs")
      slot
      .border-l.border-white.mx-3
      c-dropdown-arrow.self-center(:toggled="toggled")
</template>
