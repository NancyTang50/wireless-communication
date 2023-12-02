module.exports = {
    darkMode: 'class',
    future: {
        removeDeprecatedGapUtilities: true,
        purgeLayersByDefault: false,
    },
    content: [
        "./Pages/**/*.{razor,html,cshtml}",
        "./Shared/**/*.{razor,html,cshtml}",
        "./wwwroot/index.html"
    ],
    theme: {
        extend: {
            backgroundImage: {},
            backgroundSize: {
                '100%': '100%',
                '75%': '75%',
                '50%': '50%',
                '25%': '25%'
            },
            colors: {
                'ultra-light-gray': '#fcfcfc',
                'discord': '#7289DA'
            },
            height: {
                '88': '22rem',
                '104': '26rem',
                '112': '28rem',
                '120': '30rem',
                '128': '32rem',
                '136': '34rem',
            },
            width: {
                '88': '22rem',
                '104': '26rem',
                '112': '28rem',
                '120': '30rem',
                '128': '32rem',
                '136': '34rem',
            },
            spacing: {
                '22': '5.5rem',
                '26': '6.5rem'
            }
        },
        fontFamily: {}
    },
    variants: {},
    plugins: [],
}
