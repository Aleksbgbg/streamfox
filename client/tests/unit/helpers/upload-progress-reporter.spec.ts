import { ProgressReport } from "@/helpers/progress-report";
import { UploadProgressReporter } from "@/helpers/upload-progress-reporter";

describe("UploadProgressReporter", () => {
  describe.each([
    [3849, 10000, 0.3849],
    [79815, 100000, 0.7982]
  ])("given loaded data and total", (loadedBytes, totalBytes, expectedUploadedFraction) => {
    test("calculates transaction uploadedFraction", () => {
      const uploadProgressReporter = new UploadProgressReporter({
        beginRecording() { },
        reportElapsedTimeSeconds(): number {
          return 0;
        }
      });
      let reportedProgress: ProgressReport = { uploadedFraction: 0, dataRateBytesPerSecond: 0 };
      const reportProgress = uploadProgressReporter.createProgressReporter((progressReport: ProgressReport) => {
        reportedProgress = progressReport;
      });

      reportProgress({ loaded: loadedBytes, total: totalBytes });

      expect(reportedProgress.uploadedFraction).toBeCloseTo(expectedUploadedFraction);
    });
  });

  describe.each([
    [5, 250_000, 50_000],
    [12, 120_000, 10_000]
  ])("given elapsed time and transferred data", (elapsedTimeSeconds, transferredBytes, expectedDataRate) => {
    test("calculates data rate from elapsed time", () => {
      const uploadProgressReporter = new UploadProgressReporter({
        beginRecording() { },
        reportElapsedTimeSeconds(): number {
          return elapsedTimeSeconds;
        }
      });
      let reportedProgress: ProgressReport = { uploadedFraction: 0, dataRateBytesPerSecond: 0 };
      const reportProgress = uploadProgressReporter.createProgressReporter((progressReport: ProgressReport) => {
        reportedProgress = progressReport;
      });

      reportProgress({ loaded: transferredBytes, total: transferredBytes + 1 });

      expect(reportedProgress.dataRateBytesPerSecond).toBe(expectedDataRate);
    });
  });

  test("begins recording before reporting elapsed time", () => {
    let isRecording = false;
    const uploadProgressReporter = new UploadProgressReporter({
      beginRecording() {
        isRecording = true;
      },
      reportElapsedTimeSeconds(): number {
        return 0;
      }
    });
    let reportedProgress: ProgressReport = { uploadedFraction: 0, dataRateBytesPerSecond: 0 };
    const reportProgress = uploadProgressReporter.createProgressReporter((progressReport: ProgressReport) => {
      reportedProgress = progressReport;
    });

    reportProgress({ loaded: 0, total: 1 });

    expect(isRecording).toBe(true);
  });
});