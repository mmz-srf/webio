export class PropertyChangeEvent {
    constructor(
        public property: string,
        public newValue: string,
        public entity: string,
        public device: string,
        public entityType: string) { }
}
