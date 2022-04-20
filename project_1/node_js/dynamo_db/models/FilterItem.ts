import { FilterOperand } from './FilterOperand';
import { LogicalConnection } from './LogicalConnection';

export interface FilterItem {
  field: string;
  operand: FilterOperand;
  value: string | number | boolean;
  connection: LogicalConnection;
}
