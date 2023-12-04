import {Injectable} from '@angular/core';
import {DataFieldDto} from '../data/models/data-field-dto';
import {FactoryType} from '../data/models/factory-type';

function flatten(array): string[] {
  return Array.isArray(array) ? [].concat.apply([], array.map(flatten)) : array;
}

interface SelectOptionsFactory {
  generate(referenceValue: string): string[];
}

export class SwitchPortFactory implements SelectOptionsFactory {
  private static coreSwitchPorts = flatten(this.range(1, 8)
    .map(a => this.range(1, 36)
      .map(b => this.range(1, 4)
        .map(c => `${a}_${b}_${c}`))));
  private static leafSwitchPorts = this.range(1, 48);

  private static range(start: number, end: number): string[] {
    return [...Array(end - start + 1)].map((_, i) => `${i + start}`);
  }

  generate(referenceValue: string): string[] {
    const invalidSwitch = ['Switch name needs to contain \'leaf\' or \'core\'!'];
    if (!referenceValue) {
      return invalidSwitch;
    }

    if (referenceValue.toLowerCase().includes('leaf')) {
      return SwitchPortFactory.leafSwitchPorts;
    }

    if (referenceValue.toLowerCase().includes('core')) {
      return SwitchPortFactory.coreSwitchPorts;
    }
    return invalidSwitch;
  }
}

@Injectable({
  providedIn: 'root'
})
export class SelectionOptionsService {

  constructor() {
  }

  public getSelectableValues(selectionField: DataFieldDto, referenceValue: string) {
    if (!selectionField.selectableValuesFactory) {
      return [''].concat(selectionField.selectableValues);
    }

    const factory = this.getFactory(selectionField);
    return factory.generate(referenceValue);
  }

  private getFactory(selectionField: DataFieldDto): SelectOptionsFactory {
    switch (selectionField.selectableValuesFactory.type) {
      case FactoryType.SwitchPortFactory:
        return new SwitchPortFactory();
    }
    this.failNever(selectionField.selectableValuesFactory.type);
  }

  private failNever(arg): never {
    throw new Error(`unknown selection field type ${arg}`);
  }
}
