import { VolumeSettingsStore } from "@/services/volume-settings-store";
import { localGet, localSet } from "@/utils/local-storage";

const VOLUME = "volume";

export class SettingsStore implements VolumeSettingsStore {
  public getVolume(): number {
    return SettingsStore.getSettingAsNumber(VOLUME, 0.5);
  }

  public setVolume(value: number): void {
    localSet(VOLUME, value.toString());
  }

  private static getSettingAsNumber(name: string, defaultValue: number): number {
    return Number.parseFloat(localGet(name, defaultValue.toString()));
  }
}
