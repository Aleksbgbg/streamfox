<template lang="pug">
.flex.justify-center
  form.bg-theme-light.rounded-lg.p-5.m-5
    c-form-input(title="Title" placeholder="(optional)")
    c-form-textarea(title="Description" placeholder="(optional)")
    .flex.flex-col
      label Tags
      .bg-white.text-black.rounded.px-2.py-3
        .inline
          span.inline-block.rounded-full.bg-gray-500.text-white.shadow-xs.hover-shadow.mr-2.px-2.py-1(v-for="tag of tags") {{ tag }}
        input.inline.bg-transparent.text-black(v-model="tagText" @keydown="tagKeyDown" @input="tagInput")
      span.text-orange-300.text-sm(v-show="tags.length >= 5 && tagText.length > 0") You can only set 5 tags per video.
</template>

<script>
import FormInputComponent from "@/components/forms/input.vue";
import FormTextareaComponent from "@/components/forms/textarea.vue";

function popTag(tags) {
  const tag = tags[tags.length - 1];
  tags.splice(tags.length - 1, 1);
  return tag;
}

function fixSeparators(tag) {
  return tag.replaceAll(" ", "-").replaceAll(/-+/g, "-");
}

function filterInvalidCharacters(tag) {
  return (tag.match(/[a-z0-9\- ]/g) || []).join("").trim();
}

function processTag(tag) {
  return fixSeparators(filterInvalidCharacters(tag.toLowerCase()));
}

export default {
  components: {
    "c-form-input": FormInputComponent,
    "c-form-textarea": FormTextareaComponent,
  },
  data() {
    return {
      tags: [],
      tagText: "",
    };
  },
  methods: {
    tagInput(event) {
      if (event.data === "," && this.tags.length < 5) {
        const tag = processTag(this.tagText.substring(0, this.tagText.length - 1));

        if (tag.length > 0) {
          this.tags.push(tag);
        }

        this.tagText = "";
      }
    },
    tagKeyDown(event) {
      if (event.keyCode === 8 && this.tagText === "") {
        this.tagText = popTag(this.tags);
        event.preventDefault();
      }
    },
  },
};
</script>
