import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'vertical-nav-tab',
  templateUrl: './vertical-nav-tab.component.html',
  styleUrls: ['./vertical-nav-tab.component.scss']
})
export class VerticalNavTabComponent implements OnInit {

  @Input() smallMenu: boolean;
  @Input() tabTitle: string;

  constructor() { }

  ngOnInit() {
  }

}
