import { Component, OnInit, Input } from '@angular/core';
import { SolutionTypeTag } from '../../../models/solution-type-tag';

export class TitleData {
  constructor(public title: string, public tag: SolutionTypeTag, public index: number) {}
}

@Component({
  selector: 'solution-display-item',
  templateUrl: './solution-display-item.component.html',
  styleUrls: ['./solution-display-item.component.scss']
})
export class SolutionDisplayItemComponent implements OnInit {

  @Input() title: string;
  @Input() titleTag: SolutionTypeTag;
  @Input() index: number;
  titleData: TitleData;
  isSelected: boolean;

  constructor() { }

  ngOnInit() {
    this.titleData = new TitleData(this.title, this.titleTag, this.index);
  }

}
