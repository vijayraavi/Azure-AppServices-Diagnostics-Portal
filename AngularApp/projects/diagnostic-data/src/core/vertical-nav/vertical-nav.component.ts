import { Component, OnInit, Input, ContentChild, TemplateRef } from '@angular/core';

@Component({
  selector: 'vertical-nav',
  templateUrl: './vertical-nav.component.html',
  styleUrls: ['./vertical-nav.component.scss']
})
export class VerticalNavComponent implements OnInit {

  @Input() smallMenu: boolean;
  @Input() navTitle: string;
  @Input() showTitle = true;
  @ContentChild(TemplateRef) template: TemplateRef;
  childContext: {};

  constructor() { }

  ngOnInit() {
    this.childContext = {$implicit: this.smallMenu, smallMenu: this.smallMenu}
  }

}
