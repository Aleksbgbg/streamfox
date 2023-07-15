import { createProgressReporter } from "@/utils/upload-progress-reporter";

describe("UploadProgressReporter", () => {
  describe.each([
    [3849, 10000, 0.3849],
    [79815, 100000, 0.7982],
  ])(
    "given uploadedBytes and totalBytes",
    (uploadedBytes, totalBytes, expectedUploadedFraction) => {
      test("calculates uploadedFraction", () => {
        let reportedProgress = { uploadedFraction: 0, dataRateBytesPerSec: 0 };
        const reportProgress = createProgressReporter(
          function (progressReport) {
            reportedProgress = progressReport;
          },
          {
            beginRecording() {},
            elapsedTimeSecs(): number {
              return 0;
            },
          }
        );

        reportProgress({ uploadedBytes, totalBytes });

        expect(reportedProgress.uploadedFraction).toBeCloseTo(expectedUploadedFraction);
      });
    }
  );

  describe.each([
    [5, 250_000, 50_000],
    [12, 120_000, 10_000],
  ])(
    "given elapsed time and transferred data",
    (elapsedTimeSecs, transferredBytes, expectedDataRate) => {
      test("calculates upload data rate", () => {
        let reportedProgress = { uploadedFraction: 0, dataRateBytesPerSec: 0 };
        const reportProgress = createProgressReporter(
          function (progressReport) {
            reportedProgress = progressReport;
          },
          {
            beginRecording() {},
            elapsedTimeSecs(): number {
              return elapsedTimeSecs;
            },
          }
        );

        reportProgress({ uploadedBytes: transferredBytes, totalBytes: transferredBytes + 1 });

        expect(reportedProgress.dataRateBytesPerSec).toBe(expectedDataRate);
      });
    }
  );

  test("begins recording before reporting elapsed time", () => {
    let isRecording = false;
    const reportProgress = createProgressReporter(function (_) {}, {
      beginRecording() {
        isRecording = true;
      },
      elapsedTimeSecs(): number {
        return 0;
      },
    });

    reportProgress({ uploadedBytes: 0, totalBytes: 1 });

    expect(isRecording).toBe(true);
  });

  test("accepts multiple progress reports", () => {
    let reportedProgress = { uploadedFraction: 0, dataRateBytesPerSec: 0 };
    let elapsedTime = 0;
    const reportProgress = createProgressReporter(
      function (progressReport) {
        reportedProgress = progressReport;
      },
      {
        beginRecording() {},
        elapsedTimeSecs(): number {
          return elapsedTime;
        },
      }
    );

    elapsedTime = 3;
    reportProgress({ uploadedBytes: 0, totalBytes: 300_000 });
    elapsedTime = 6;
    reportProgress({ uploadedBytes: 300_000, totalBytes: 300_000 });

    expect(reportedProgress.dataRateBytesPerSec).toEqual(50_000);
  });
});
