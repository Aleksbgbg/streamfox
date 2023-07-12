import { SettingsStore } from "@/services/settings-store";
import { type VolumeSettingsStore } from "@/services/volume-settings-store";

const settingsStore = new SettingsStore();

export const volumeSettingsStore: VolumeSettingsStore = settingsStore;
