export interface VolumeSettingsStore {
  getVolume(): number;

  setVolume(value: number): void;
}
