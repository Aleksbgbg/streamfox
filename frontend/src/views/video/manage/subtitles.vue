<script setup lang="ts">
import { type Ref, onBeforeMount, reactive, ref } from "vue";
import CErrors from "@/components/forms/errors.vue";
import CFormInput from "@/components/forms/input.vue";
import CSuccess from "@/components/forms/success.vue";
import CFormTextarea from "@/components/forms/textarea.vue";
import CSpinner from "@/components/spinner.vue";
import CTab from "@/components/tab-control/tab.vue";
import CTabs from "@/components/tab-control/tabs.vue";
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
import { panic } from "@/utils/panic";

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

const extractSuccess = ref<InstanceType<typeof CSuccess> | null>(null);
const saveSuccess = ref<InstanceType<typeof CSuccess> | null>(null);
const deleteSuccess = ref<InstanceType<typeof CSuccess> | null>(null);

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
    return;
  }

  extractErr.value = emptyApiErr();

  subtitlesExtracted.value = true;

  const count = response.value().length;
  if (count === 0) {
    extractSuccess.value?.show("No embedded subtitles were found.");
  } else {
    extractSuccess.value?.show(`Extracted ${response.value().length} subtitle tracks!`);

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
    return;
  }

  subtitle.content = response.value();
}

async function create(content = "") {
  const response = await createSubtitle(props.videoId, content);

  if (!response.success()) {
    createErr.value = response.err();
    return;
  }

  createErr.value = emptyApiErr();

  const subtitle = response.value();
  subtitles.push({ id: subtitle.id, name: subtitle.name, content: "" });
}

function createFromFile() {
  const file = fileInput.value?.files?.[0] ?? panic("no file found");

  const reader = new FileReader();
  reader.onloadend = async function () {
    await create(reader.result?.toString() ?? panic("no file read result"));
  };
  reader.readAsBinaryString(file);
}

async function save(subtitle: SubtitleWithContent) {
  const response = await updateSubtitle(props.videoId, subtitle.id, {
    name: subtitle.name,
    content: subtitle.content,
  });

  if (!response.success()) {
    saveErr.value = response.err();
    return;
  }

  saveErr.value = emptyApiErr();

  saveSuccess.value?.show();
}

async function deleteSub(subtitle: SubtitleWithContent, index: number) {
  const response = await deleteSubtitle(props.videoId, subtitle.id);

  if (!response.success()) {
    deleteErr.value = response.err();
    return;
  }

  deleteErr.value = emptyApiErr();

  subtitles.splice(index, 1);

  deleteSuccess.value?.show(`Deleted subtitle track titled '${subtitle.name}'.`);
}
</script>

<template lang="pug">
.flex.flex-col.h-full.max-h-full.px-5.pb-5
  .flex.justify-center.mb-5(v-if="!subtitlesExtracted")
    .rounded.bg-aurora-purple.px-5.py-2
      p.flex.items-center.mb-3
        span.text-4xl.font-bold.mr-5 !
        span.text-center Your video may have embedded subtitles.
      button(
        v-show="!extracting"
        class="flex justify-center items-center rounded bg-aurora-purple-600 hover:bg-aurora-purple-700 w-full p-2"
        @click="extract"
      ) Extract them!
      p.text-center(v-show="extracting")
        c-spinner.mr-2
        span Extracting...
  p.text-aurora-yellow-300.text-center(v-show="subtitles.length === 0") No subtitles here. Add some!
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
          button(
            class="rounded bg-aurora-red hover:bg-aurora-red-700 px-2 py-1 mr-3"
            @click="deleteSub(subtitle, index)"
          ) Delete
          button(
            class="rounded bg-frost-blue hover:bg-frost-deep px-2 py-1"
            @click="save(subtitle)"
          ) Save
  .flex
    button(
      class="rounded bg-frost-blue hover:bg-frost-deep px-2 py-1 m-2"
      @click="create()"
    )
      i.bi-plus.text-xl
    button(class="rounded bg-frost-blue hover:bg-frost-deep px-2 py-1 m-2")
      label(for="file-input" class="hover:cursor-pointer")
        i.bi-file-earmark-plus.text-xl
      input.hidden#file-input(
        type="file"
        ref="fileInput"
        @change="createFromFile"
      )
  c-success(:timeout="3000" ref="extractSuccess")
  c-success(message="Saved!" ref="saveSuccess")
  c-success(:timeout="3000" ref="deleteSuccess")
  c-errors(center :errors="extractErr.generic")
  c-errors(center :errors="createErr.generic")
  c-errors(center :errors="saveErr.generic")
  c-errors(center :errors="deleteErr.generic")
</template>
