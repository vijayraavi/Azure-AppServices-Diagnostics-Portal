import { Component, Input, QueryList, ContentChildren, AfterContentInit } from '@angular/core';
import { TitleData, SolutionDisplayItemComponent } from './solution-display-item/solution-display-item.component';

@Component({
  selector: 'solution-display',
  templateUrl: './solution-display.component.html',
  styleUrls: ['./solution-display.component.scss']
})
export class SolutionDisplayComponent implements AfterContentInit {

  @Input() showTitle = true;
  @ContentChildren(SolutionDisplayItemComponent) listItems: QueryList<SolutionDisplayItemComponent>;
  titles: TitleData[] = [];
  selectedItem: TitleData;

  select(item: TitleData) {
    this.listItems.forEach(
      listItem => listItem.isSelected = listItem.title === item.title
    );
    this.selectedItem = item;
  }

  ngAfterContentInit() {
    this.listItems.forEach((item, index) => {
      item.isSelected = index === 0;
      this.titles.push(item.titleData)
    });

    this.selectedItem = this.titles.length > 0 ? this.titles[0] : null;
  }

}
