// Forwards /api to the gateway so the dev server URL shown in the dashboard works on its own
const target =
  process.env['services__gateway__http__0'] ??
  process.env['services__gateway__https__0'] ??
  'http://localhost:5100';

export default {
  '/api': {
    target,
    secure: false,
  },
};
