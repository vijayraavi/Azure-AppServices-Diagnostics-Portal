/*
  This file can be replaced during build by using the `fileReplacements` array.
  `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
  The list of file replacements can be found in `angular.json`.
*/

export const environment = {
  production: false,
  backendHost: 'http://localhost:5000/',
  useApplensBackend: true,
  authServiceToken: "AVQAq/8KAAAAF3THT0CQFXjQnkpFhSwn7QPv9Vb2k4/fLOoFSUSztmMfPvRycpyBH7vfPSbqeFe2mS+n17I+jsXxl+s5XIBEJ1LPuP07/qgL7XIpUJcmTsY="  ,
  authServiceResourceId: "/subscriptions/0d3ae56c-deaf-4982-b514-33d016d4a683/resourceGroups/cindybuggyfn/providers/Microsoft.Web/sites/cindybuggyfn"
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
import 'zone.js/dist/zone-error';  // Included with Angular CLI.
