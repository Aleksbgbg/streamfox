export interface ConversionProgressResponse {
  isCompleted: boolean;

  currentFrame: number;

  videoDuration: number;

  doneFraction: number;

  timeElapsed: number;

  timeRemaining: number;
}