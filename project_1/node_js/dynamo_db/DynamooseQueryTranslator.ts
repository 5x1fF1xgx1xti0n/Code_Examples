import * as dynamoose from 'dynamoose';
import { Condition } from 'dynamoose/dist/Condition';

import { FilterItem } from './models/FilterItem';
import { FilterOperand } from './models/FilterOperand';
import { LogicalConnection } from './models/LogicalConnection';

export class DynamooseQueryTranslator {
  translateFilters(filters: FilterItem[]): Condition {
    let condition = new dynamoose.Condition();

    filters.forEach((f) => {
      condition = this.translateOperation(condition, f);
      condition = this.translateConnection(condition, f.connection);
    });

    return condition;
  }

  private translateOperation(condition: Condition, f: FilterItem): Condition {
    switch (f.operand) {
      case FilterOperand.Equal:
        return condition.filter(f.field).eq(f.value);
      case FilterOperand.NotEqual:
        return condition.filter(f.field).not().eq(f.value);
      case FilterOperand.GreaterThan:
        return condition.filter(f.field).gt(f.value);
      case FilterOperand.GreaterThanOrEqual:
        return condition.filter(f.field).ge(f.value);
      case FilterOperand.LessThan:
        return condition.filter(f.field).lt(f.value);
      case FilterOperand.LessThanOrEqual:
        return condition.filter(f.field).le(f.value);
      case FilterOperand.StartsWith:
        return condition.filter(f.field).beginsWith(f.value);
      case FilterOperand.Contains:
        return condition.filter(f.field).contains(f.value);
      case FilterOperand.In:
        return condition.filter(f.field).in(f.value);
      default:
        return condition;
    }
  }

  private translateConnection(condition: Condition, c: LogicalConnection): Condition {
    switch (c) {
      case LogicalConnection.And:
        return condition.and();
      case LogicalConnection.Or:
        return condition.or();
      default:
        return condition;
    }
  }
}
