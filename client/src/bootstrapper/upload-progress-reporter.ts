import { RealElapsedTime } from "@/helpers/real-elapsed-time";
import { UploadProgressReporter } from "@/helpers/upload-progress-reporter";

export const uploadProgressReporter = new UploadProgressReporter(new RealElapsedTime());