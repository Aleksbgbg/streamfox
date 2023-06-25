function formatTimes(totalSeconds: number): string {
  let string = "";

  const minutes = Math.floor(totalSeconds / 60);

  if (minutes > 0) {
    string += `${minutes}:`;
  }

  const seconds = totalSeconds % 60;

  if (seconds < 10 && minutes != 0) {
    string += "0";
  }

  string += `${seconds}`;

  if (minutes == 0) {
    string += "s";
  }

  return string;
}

describe("formats hours", () => {
  it("less than 60", () => {
    expect(formatTimes(7)).toEqual("7s");
  });

  it("more than 60", () => {
    expect(formatTimes(65)).toEqual("1:05");
  });
});
