import { VolumeSettingsStore } from "@/services/volume-settings-store";

const VOLUME = "volume";

export class SettingsStore implements VolumeSettingsStore {
  public getVolume(): number {
    return SettingsStore.getSettingAsNumber(VOLUME, 0.5);
  }

  public setVolume(value: number): void {
    SettingsStore.setSetting(VOLUME, value.toString());
  }

  private static getSettingAsNumber(name: string, defaultValue: number): number {
    return Number.parseFloat(SettingsStore.getSetting(name, defaultValue.toString()));
  }

  private static getSetting(name: string, defaultValue: string): string {
    return localStorage.getItem(name) ?? defaultValue;
  }

  private static setSetting(name: string, value: string): void {
    return localStorage.setItem(name, value);
  }
}
