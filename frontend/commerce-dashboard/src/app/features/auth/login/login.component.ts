import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { ApiError, LoginRequest } from '../../../core/models/auth.models';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  errorMessage = '';
  isSubmitting = false;

  loginForm;

  constructor(
    private readonly formBuilder: FormBuilder,
    private readonly authService: AuthService
  ) {
    this.loginForm = this.formBuilder.nonNullable.group({
      email: ['', [
        Validators.required,
        Validators.email
      ]],
      password: ['', Validators.required]
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.errorMessage = '';
    this.isSubmitting = true;

    const request: LoginRequest = {
      email: this.loginForm.controls.email.value,
      password: this.loginForm.controls.password.value
    };

    this.authService.login(request).subscribe({
      next: () => {
        this.isSubmitting = false;
      },
      error: (error) => {
        this.isSubmitting = false;

        const apiError = error.error as ApiError;

        this.errorMessage =
          apiError.detail ?? 'Login failed.';
      }
    });
  }
}