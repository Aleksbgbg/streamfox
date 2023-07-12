import { RealElapsedTime } from "@/utils/real-elapsed-time";
import { UploadProgressReporter } from "@/utils/upload-progress-reporter";

export const uploadProgressReporter = new UploadProgressReporter(new RealElapsedTime());
