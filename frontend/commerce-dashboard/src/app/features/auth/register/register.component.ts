import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import {FormBuilder, ReactiveFormsModule, Validators} from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import {ApiError, RegisterRequest} from '../../../core/models/auth.models';


@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})

export class RegisterComponent {
  errorMessage = '';
  isSubmitting = false;
  registrationSuccessful = false;

  registerForm;

  constructor(
    private readonly formBuilder: FormBuilder,
    private readonly authService: AuthService
  ) {
    this.registerForm = this.formBuilder.nonNullable.group({
      merchantName: ['', Validators.required],
      email: ['', [
        Validators.required,
        Validators.email
      ]],
      password: ['', [
        Validators.required,
        Validators.minLength(6)
      ]]
    });
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.errorMessage = '';
    this.registrationSuccessful = false;
    this.isSubmitting = true;

    const request: RegisterRequest = {
      merchantName: this.registerForm.controls.merchantName.value,
      email: this.registerForm.controls.email.value,
      password: this.registerForm.controls.password.value
    };

    this.authService.register(request).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.registrationSuccessful = true;
      },
      error: (error) => {
        this.isSubmitting = false;

        const apiError = error.error as ApiError;

        this.errorMessage =
          apiError.detail ?? 'Registration failed.';
      }
    });
  }
}

//Creates and validates the merchant registration form. 
//Sends valid registration data to the API and handles success 
//or error responses.