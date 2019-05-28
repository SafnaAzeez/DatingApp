import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css']
})
export class NavbarComponent implements OnInit {
  model: any = {};

  constructor(public authService: AuthService, private alertifyService: AlertifyService) { }

  ngOnInit() {
  }
  login() {
    this.authService.login(this.model).subscribe(res => {
      this.alertifyService.success('logged in successfully');
    }, err => this.alertifyService.error(err));
  }

  loggedIn() {
    return this.authService.loggedIn();
  }

  logout() {
    this.alertifyService.message('You have logged out successfully');
    localStorage.removeItem('token');
  }
}
