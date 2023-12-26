<script setup lang="ts">
import { type Ref, reactive, ref } from "vue";
import { useRouter } from "vue-router";
import CButton from "@/components/button.vue";
import CFormInput from "@/components/forms/input.vue";
import CFormOption from "@/components/forms/select/option.vue";
import CFormSelect from "@/components/forms/select/select.vue";
import CFormLayout from "@/components/layout/form.vue";
import { useToaster } from "@/components/toasts/toaster";
import { type CreateLiveRoomInfo, Visibility, createLiveRoom } from "@/endpoints/live";
import { type ApiErr, emptyApiErr } from "@/endpoints/request";
import type { User } from "@/endpoints/user";
import { useUserStore } from "@/store/user";
import { type Option } from "@/types/option";
import { toPossessive } from "@/utils/strings";

const router = useRouter();

const userStore = useUserStore();

const toaster = useToaster();

const room: CreateLiveRoomInfo = reactive({
  name: "",
  visibility: Visibility.Public,
});

const err: Ref<ApiErr<CreateLiveRoomInfo>> = ref(emptyApiErr());

function setRoomNameFromUser(user: Option<User>) {
  room.name = `${toPossessive(user.mapOrElse((user) => user.username, "Anonymous"))} Room`;
}

setRoomNameFromUser(userStore.user);

if (userStore.user.isNone()) {
  const unsubscribe = userStore.$subscribe((_, state) => {
    setRoomNameFromUser(state.user);
    unsubscribe();
  });
}

async function submit() {
  const response = await createLiveRoom(room);

  if (!response.success()) {
    err.value = response.err();
    toaster.failureAll(err.value.generic);
    return;
  }

  const id = response.value().id;
  router.push({ name: "live-room", params: { id } });
}
</script>

<template lang="pug">
c-form-layout(title="Create Live Room")
  form.flex.flex-col.items-center.gap-3.my-5(@submit.prevent="submit")
    c-form-input(
      center
      title="Name"
      v-model="room.name"
      :errors="err.specific.name"
    )
    c-form-select(
      center
      title="Visibility"
      v-model.number="room.visibility"
      :errors="err.specific.visibility"
    )
      c-form-option(title="Public" :value="Visibility.Public")
      c-form-option(title="Unlisted" :value="Visibility.Unlisted")
    c-button Create
</template>
