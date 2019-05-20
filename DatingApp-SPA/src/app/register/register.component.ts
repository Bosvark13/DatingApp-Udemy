import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';

@Component({
    selector: 'app-register',
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.scss']
})
/** register component*/
export class RegisterComponent implements OnInit {
  @Input() valuesFromHome: any;
  @Output() cancelRegister = new EventEmitter();

  model: any = {};

  ngOnInit(): void { }

  /** register ctor */
  constructor(private authService: AuthService, private alertify: AlertifyService) {
      
  }

  register() {
    this.authService.register(this.model).subscribe(() => {
      this.alertify.success('Registration successfull');
    }, error => {
      this.alertify.error(error);
    });
  }

  cancel() {
    this.cancelRegister.emit(false);
    this.alertify.message('cancelled');
  }
}