import dayjs from "dayjs";
import relativeTime from "dayjs/plugin/relativeTime";

dayjs.extend(relativeTime);

const SECS_PER_DAY = 60 * 60 * 24;
const SECS_PER_HOUR = 60 * 60;
const SECS_PER_MINUTE = 60;

export function secsToDurationString(secs: number): string {
  const days = Math.floor(secs / SECS_PER_DAY);
  secs %= SECS_PER_DAY;

  const hours = Math.floor(secs / SECS_PER_HOUR);
  secs %= SECS_PER_HOUR;

  const mins = Math.floor(secs / SECS_PER_MINUTE);
  secs %= SECS_PER_MINUTE;

  let string = "";
  let isLeading = true;
  for (const formatter of [
    {
      time: days,
      required: false,
      minLen: 1,
      maxLen: Number.MAX_VALUE,
    },
    {
      time: hours,
      required: false,
      minLen: 1,
      maxLen: 2,
    },
    {
      time: mins,
      required: true,
      minLen: 1,
      maxLen: 2,
    },
    {
      time: secs,
      required: true,
      minLen: 2,
      maxLen: 2,
    },
  ]) {
    if (isLeading && formatter.time === 0 && !formatter.required) {
      continue;
    }

    if (!isLeading) {
      string += ":";
    }
    string += formatter.time
      .toString()
      .padStart(isLeading ? formatter.minLen : formatter.maxLen, "0");

    isLeading = false;
  }

  return string;
}

export function dateToElapsedTimeString(date: Date): string {
  return dayjs(date).fromNow();
}

export function toLowerCamelCase(string: string): string {
  return (string.charAt(0).toLowerCase() + string.slice(1)).replace(" ", "");
}
