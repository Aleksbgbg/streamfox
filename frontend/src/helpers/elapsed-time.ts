export interface ElapsedTime {
  beginRecording(): void;

  reportElapsedTimeSeconds(): number;
}
