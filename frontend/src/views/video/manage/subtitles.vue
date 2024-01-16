<script setup lang="ts">
import { type Ref, onBeforeMount, reactive, ref } from "vue";
import CButton from "@/components/button.vue";
import CEmptyCollection from "@/components/empty-collection.vue";
import CFormInput from "@/components/forms/input.vue";
import CFormTextarea from "@/components/forms/textarea.vue";
import CIcon from "@/components/icon.vue";
import CSpinner from "@/components/spinner.vue";
import CTab from "@/components/tab-control/tab.vue";
import CTabs from "@/components/tab-control/tabs.vue";
import { useToaster } from "@/components/toasts/toaster";
import { type ApiErr, emptyApiErr } from "@/endpoints/request";
import {
  type Subtitle,
  type UpdateSubtitleInfo,
  createSubtitle,
  deleteSubtitle,
  extractSubtitles,
  getAllSubtitles,
  getSubtitleContent,
  getSubtitleInfo,
  updateSubtitle,
} from "@/endpoints/subtitle";
import type { VideoId } from "@/endpoints/video";
import { mapCopy } from "@/utils/arrays";
import { check } from "@/utils/null";

const toaster = useToaster();

const props = defineProps<{
  videoId: VideoId;
}>();

const fileInput: Ref<HTMLInputElement | null> = ref(null);

const subtitlesExtracted = ref(true);
const extracting = ref(false);

interface SubtitleWithContent extends Subtitle {
  content: string;
}
const subtitles: SubtitleWithContent[] = reactive([]);

onBeforeMount(async () => {
  const [info, subs] = await Promise.all([
    getSubtitleInfo(props.videoId),
    getAllSubtitles(props.videoId),
  ]);

  if (info.success()) {
    subtitlesExtracted.value = info.value().subtitlesExtracted;
  }

  if (subs.success()) {
    mapCopy(subtitles, subs.value(), (subtitle) => ({
      id: subtitle.id,
      name: subtitle.name,
      content: "",
    }));
  }
});

const extractErr: Ref<ApiErr<void>> = ref(emptyApiErr());
const createErr: Ref<ApiErr<void>> = ref(emptyApiErr());
const saveErr: Ref<ApiErr<UpdateSubtitleInfo>> = ref(emptyApiErr());
const deleteErr: Ref<ApiErr<void>> = ref(emptyApiErr());

async function extract() {
  extracting.value = true;
  const response = await extractSubtitles(props.videoId);
  extracting.value = false;

  if (!response.success()) {
    extractErr.value = response.err();
    toaster.failureAll(extractErr.value.generic);
    return;
  }

  extractErr.value = emptyApiErr();

  subtitlesExtracted.value = true;

  const count = response.value().length;
  if (count === 0) {
    toaster.success("No embedded subtitles were found.");
  } else {
    toaster.success(`Extracted ${response.value().length} subtitle track(s)!`);

    mapCopy(subtitles, response.value(), (subtitle) => ({
      id: subtitle.id,
      name: subtitle.name,
      content: "",
    }));
  }
}

async function loadSubtitle(subtitle: SubtitleWithContent) {
  const response = await getSubtitleContent(props.videoId, subtitle.id);

  if (!response.success()) {
    toaster.failureAll(response.err().generic);
    return;
  }

  subtitle.content = response.value();
}

async function create(content = "") {
  const response = await createSubtitle(props.videoId, content);

  if (!response.success()) {
    createErr.value = response.err();
    toaster.failureAll(createErr.value.generic);
    return;
  }

  createErr.value = emptyApiErr();

  const subtitle = response.value();
  subtitles.push({ id: subtitle.id, name: subtitle.name, content: "" });
}

function createFromFile() {
  const file = check(fileInput.value?.files?.[0]);

  const reader = new FileReader();
  reader.onloadend = async function () {
    await create(check(reader.result?.toString()));
  };
  reader.readAsText(file);
}

async function save(subtitle: SubtitleWithContent) {
  const response = await updateSubtitle(props.videoId, subtitle.id, {
    name: subtitle.name,
    content: subtitle.content,
  });

  if (!response.success()) {
    saveErr.value = response.err();
    toaster.failureAll(saveErr.value.generic);
    return;
  }

  saveErr.value = emptyApiErr();
  toaster.success("Subtitle details saved!");
}

async function deleteSub(subtitle: SubtitleWithContent, index: number) {
  const response = await deleteSubtitle(props.videoId, subtitle.id);

  if (!response.success()) {
    deleteErr.value = response.err();
    toaster.failureAll(deleteErr.value.generic);
    return;
  }

  deleteErr.value = emptyApiErr();
  subtitles.splice(index, 1);
  toaster.success(`Deleted subtitle track titled '${subtitle.name}'.`);
}
</script>

<template lang="pug">
.flex.flex-col.h-full.max-h-full.px-5.pb-5
  .flex.justify-center.mb-5(v-if="!subtitlesExtracted")
    .rounded.bg-aurora-purple.px-5.py-2
      p.flex.items-center.mb-3
        span.text-4xl.font-bold.mr-5 !
        span.text-center Your video may have embedded subtitles.
      c-button.w-full(
        v-show="!extracting"
        theme="purple"
        :colors="{ default: 'bg-aurora-purple-600' }"
        @click="extract"
      ) Extract them!
      p.text-center(v-show="extracting")
        c-spinner.mr-2
        span Extracting...
  c-empty-collection(:collection="subtitles" empty="No subtitles here. Add some!")
    c-tabs(vertical)
      c-tab(
        v-for="(subtitle, index) of subtitles"
        :key="index"
        :title="subtitle.name"
        @selected.once="loadSubtitle(subtitle)"
      )
        .flex.flex-col.gap-3.h-full
          c-form-input(
            title="Name"
            v-model="subtitle.name"
            :errors="saveErr.specific.name"
          )
          c-form-textarea(
            title="Content"
            placeholder="Provide your subtitles in WebVTT format."
            v-model="subtitle.content"
            :errors="saveErr.specific.content"
          )
          .flex.justify-end
            c-button.mr-3(theme="red" @click="deleteSub(subtitle, index)") Delete
            c-button(@click="save(subtitle)") Save
  .flex.gap-3
    c-button(padding="small" @click="create()")
      c-icon.text-xl(name="plus")
    c-button(padding="small" for="file-input" class="hover:cursor-pointer")
      c-icon.text-xl(name="file-earmark-plus")
      input.hidden#file-input(
        type="file"
        ref="fileInput"
        @change="createFromFile"
      )
</template>
