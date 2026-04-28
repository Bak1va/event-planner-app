import { Routes } from '@angular/router';
import { AuthPageComponent } from '../auth-page/auth-page.component';
import { WelcomePageComponent } from '../welcome-page/welcome-page.component';

export const routes: Routes = [
	{
		path: '',
		component: WelcomePageComponent
	},
	{
		path: 'auth',
		component: AuthPageComponent
	},
	{
		path: '**',
		redirectTo: ''
	}
];
