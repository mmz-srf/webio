import {StreamDto} from '../data/models/stream-dto';

const tagSeparator = ';';

function addTag(stream: StreamDto, tagName: string): void {
  const tags = new Set(getTags(stream));
  tags.add(tagName);
  setTags(stream, Array.from(tags));
}

export function setTag(stream: StreamDto, tagName: string, value: boolean): void {
  if (!value) {
    removeTag(stream, tagName);
  } else {
    addTag(stream, tagName);
  }
}

function removeTag(stream: StreamDto, tagName: string): void {
  setTags(stream, getTags(stream).filter(t => t !== tagName));
}

function getTags(stream: StreamDto): string[] {
  if (stream.properties.Tags && stream.properties.Tags.value) {
    return stream.properties.Tags.value.split(tagSeparator);
  }
  return [];
}

export function setTags(stream: StreamDto, tagNames: string[]): void {
  if (!stream.properties.Tags) {
    stream.properties.Tags = {dirty: false, inherited: false, value: ''};
  }
  stream.properties.Tags.value = tagNames.join(tagSeparator);
}
